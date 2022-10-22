import '@stackoverflow/stacks';
import '../../less/site.less';

document.addEventListener("DOMContentLoaded", () => {
    console.log('hello world! <3');
})

document.addEventListener("submit", (ev: SubmitEvent) => {
    if (ev.target instanceof HTMLFormElement) {
        if (ev.target.dataset.formReplaceWithResult === "true") {
            ev.preventDefault();
            ev.stopImmediatePropagation();

            // https://stackoverflow.com/a/46642899/11765486
            const data = new URLSearchParams();
            const formData = new FormData(ev.target);
            formData.forEach((v: FormDataEntryValue, k: string, p: FormData) => {
                data.append(k, v.toString());
            });

            fetch(ev.target.action, { method: 'post', body: data })
                .then((r: Response) => r.text())
                .then((html) => {
                    // this was erroring without the check
                    if (ev.target instanceof HTMLFormElement) {
                        ev.target.outerHTML = html;
                    }
                });

            return false;
        }
    }

    // ignore, continue on as normal
    return true;
});